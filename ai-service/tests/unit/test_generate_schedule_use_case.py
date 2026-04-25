from __future__ import annotations

from datetime import datetime

import pytest

from application import GenerateScheduleCommand, GenerateScheduleUseCase
from application.ports import RankedTask
from domain.schedule import (
    PlanningTask,
    PlanningTaskStatus,
    UnscheduledTaskReason,
)


class FakePlannerGateway:
    def __init__(self, ranked_tasks: list[RankedTask] | None = None, *, should_fail: bool = False) -> None:
        self._ranked_tasks = ranked_tasks or []
        self._should_fail = should_fail

    async def rank_tasks(self, *, tasks, user_state):
        if self._should_fail:
            raise RuntimeError("planner failed")

        return self._ranked_tasks, "fake-model"


def make_task(
    *,
    task_id: str,
    title: str,
    remaining_minutes: int,
    priority: int = 5,
    status: PlanningTaskStatus = PlanningTaskStatus.TODO,
) -> PlanningTask:
    return PlanningTask(
        id=task_id,
        title=title,
        estimated_minutes=max(remaining_minutes, 1),
        remaining_minutes=remaining_minutes,
        priority=priority,
        status=status,
    )


@pytest.mark.asyncio
async def test_generate_schedule_uses_model_ranking() -> None:
    use_case = GenerateScheduleUseCase(
        planner_gateway=FakePlannerGateway(
            ranked_tasks=[
                RankedTask(task_id="task-b", score=95, reasoning="Higher urgency."),
                RankedTask(task_id="task-a", score=50, reasoning="Less urgent."),
            ]
        )
    )

    command = GenerateScheduleCommand(
        schedule_date=datetime(2026, 4, 25).date(),
        day_start_at=datetime(2026, 4, 25, 10, 0),
        available_minutes=90,
        tasks=[
            make_task(task_id="task-a", title="Task A", remaining_minutes=30),
            make_task(
                task_id="task-b",
                title="Task B",
                remaining_minutes=60,
                priority=10,
            ),
        ],
    )

    result = await use_case.execute(command)

    assert [item.task_id for item in result.schedule.scheduled] == ["task-b", "task-a"]
    assert result.schedule.summary is not None
    assert result.schedule.summary.planner_model == "fake-model"
    assert result.schedule.summary.used_fallback_ranking is False


@pytest.mark.asyncio
async def test_generate_schedule_falls_back_when_planner_fails() -> None:
    use_case = GenerateScheduleUseCase(planner_gateway=FakePlannerGateway(should_fail=True))

    command = GenerateScheduleCommand(
        schedule_date=datetime(2026, 4, 25).date(),
        day_start_at=datetime(2026, 4, 25, 10, 0),
        available_minutes=60,
        tasks=[
            make_task(
                task_id="task-low",
                title="Task Low",
                remaining_minutes=30,
                priority=1,
            ),
            make_task(
                task_id="task-high",
                title="Task High",
                remaining_minutes=30,
                priority=8,
            ),
        ],
    )

    result = await use_case.execute(command)

    assert [item.task_id for item in result.schedule.scheduled] == ["task-high", "task-low"]
    assert result.schedule.summary is not None
    assert result.schedule.summary.planner_model == "deterministic-fallback"
    assert result.schedule.summary.used_fallback_ranking is True


@pytest.mark.asyncio
async def test_generate_schedule_marks_unavailable_tasks_as_unscheduled() -> None:
    use_case = GenerateScheduleUseCase(planner_gateway=FakePlannerGateway())

    command = GenerateScheduleCommand(
        schedule_date=datetime(2026, 4, 25).date(),
        day_start_at=datetime(2026, 4, 25, 10, 0),
        available_minutes=40,
        tasks=[
            make_task(
                task_id="completed-task",
                title="Completed",
                remaining_minutes=15,
                status=PlanningTaskStatus.COMPLETED,
            ),
            make_task(task_id="zero-task", title="Zero", remaining_minutes=0),
            make_task(task_id="todo-task", title="Todo", remaining_minutes=30),
        ],
    )

    result = await use_case.execute(command)

    assert [item.task_id for item in result.schedule.scheduled] == ["todo-task"]
    assert [item.task_id for item in result.schedule.unscheduled] == [
        "completed-task",
        "zero-task",
    ]
    assert [item.reason for item in result.schedule.unscheduled] == [
        UnscheduledTaskReason.COMPLETED,
        UnscheduledTaskReason.NO_REMAINING_TIME,
    ]
