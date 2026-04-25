from __future__ import annotations

import logging
from datetime import UTC, datetime

from fastapi import APIRouter, Depends, HTTPException, status

from app.application import GenerateScheduleCommand, GenerateScheduleUseCase
from app.application.exceptions import InvalidScheduleRequestError
from app.domain.schedule import (
    PlanningTask,
    PlanningTaskStatus,
    RecentExecutionSnapshot,
    UserStateSnapshot,
)
from app.presentation.dependencies import get_generate_schedule_use_case
from app.presentation.schemas.ai_schedule import (
    GenerateScheduleRequest,
    GenerateScheduleResponse,
    ScheduleBlockResponse,
    ScheduleSummaryResponse,
    UnscheduledTaskResponse,
)

router = APIRouter(prefix="/ai/schedule", tags=["AI Schedule"])
logger = logging.getLogger(__name__)


@router.post(
    "/generate",
    response_model=GenerateScheduleResponse,
    status_code=status.HTTP_200_OK,
)
async def generate_schedule(
    request: GenerateScheduleRequest,
    use_case: GenerateScheduleUseCase = Depends(get_generate_schedule_use_case),
) -> GenerateScheduleResponse:
    logger.info(
        "AI schedule request received. schedule_date=%s tasks_count=%s recent_executions_count=%s",
        request.schedule_date,
        len(request.tasks),
        len(request.recent_executions),
    )

    command = GenerateScheduleCommand(
        schedule_date=request.schedule_date,
        day_start_at=request.day_start_at,
        available_minutes=request.available_minutes,
        day_end_at=request.day_end_at,
        user_state=(
            UserStateSnapshot(
                energy=request.user_state.energy_level,
                stress=request.user_state.stress_level,
                motivation=request.user_state.motivation_level,
                concentration=request.user_state.concentration_level,
                sleep_minutes=request.user_state.sleep_minutes,
            )
            if request.user_state is not None
            else None
        ),
        recent_executions=[
            RecentExecutionSnapshot(
                task_id=execution.task_id,
                task_title=execution.task_title,
                estimated_minutes=execution.estimated_minutes,
                actual_minutes=execution.actual_minutes,
                priority=execution.priority,
                complexity_level=execution.complexity_level,
                energy_after=execution.energy_after,
                stress_after=execution.stress_after,
                created_at=execution.created_at,
            )
            for execution in request.recent_executions
        ],
        tasks=[
            PlanningTask(
                id=task.id,
                title=task.title,
                description=task.description,
                estimated_minutes=task.estimated_minutes,
                remaining_minutes=task.remaining_minutes,
                priority=task.priority,
                status=PlanningTaskStatus(task.work_status),
                deadline_at=task.deadline_at,
                complexity_level=task.complexity_level,
            )
            for task in request.tasks
        ],
    )

    try:
        result = await use_case.execute(command)
    except InvalidScheduleRequestError as exc:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=str(exc),
        ) from exc
    except Exception as exc:
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail=f"AI planning failed: {exc}",
        ) from exc

    schedule = result.schedule
    summary = schedule.summary
    if summary is None:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Schedule summary was not generated",
        )

    logger.info(
        "AI schedule generated. scheduled_count=%s unscheduled_count=%s planner_model=%s used_fallback_ranking=%s",
        summary.scheduled_count,
        summary.unscheduled_count,
        summary.planner_model,
        summary.used_fallback_ranking,
    )

    return GenerateScheduleResponse(
        scheduled=[
            ScheduleBlockResponse(
                task_id=item.task_id,
                title=item.title,
                start_at=item.start_at,
                end_at=item.end_at,
                planned_minutes=item.planned_minutes,
                priority=item.priority,
                reasoning=item.reasoning,
            )
            for item in schedule.scheduled
        ],
        unscheduled=[
            UnscheduledTaskResponse(
                task_id=item.task_id,
                title=item.title,
                remaining_minutes=item.remaining_minutes,
                reason=item.reason.value,
                reasoning=item.reasoning,
            )
            for item in schedule.unscheduled
        ],
        summary=ScheduleSummaryResponse(
            schedule_date=summary.schedule_date,
            available_minutes=summary.available_minutes,
            planned_minutes=summary.planned_minutes,
            scheduled_count=summary.scheduled_count,
            unscheduled_count=summary.unscheduled_count,
            explanations=summary.explanations,
            planner_model=summary.planner_model,
            used_fallback_ranking=summary.used_fallback_ranking,
            generated_at=datetime.now(UTC),
        ),
    )
