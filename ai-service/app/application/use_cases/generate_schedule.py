from __future__ import annotations

from dataclasses import dataclass
from datetime import date, datetime, timedelta

from app.application.exceptions import InvalidScheduleRequestError
from app.application.ports import PlanningModelGateway, RankedTask
from app.domain.schedule import (
    GeneratedSchedule,
    PlanningTask,
    PlanningTaskStatus,
    RecentExecutionSnapshot,
    ScheduleBlock,
    ScheduleSummary,
    UnscheduledTask,
    UnscheduledTaskReason,
    UserStateSnapshot,
)


INVALID_STATUSES = {
    PlanningTaskStatus.COMPLETED,
    PlanningTaskStatus.DELETED,
}

@dataclass(slots=True, frozen=True)
class GenerateScheduleCommand:
    schedule_date: date
    day_start_at: datetime
    tasks: list[PlanningTask]
    user_state: UserStateSnapshot | None = None
    recent_executions: list[RecentExecutionSnapshot] | None = None
    available_minutes: int | None = None
    day_end_at: datetime | None = None


@dataclass(slots=True, frozen=True)
class GenerateScheduleResult:
    schedule: GeneratedSchedule


class GenerateScheduleUseCase:
    def __init__(self, planner_gateway: PlanningModelGateway) -> None:
        self._planner_gateway = planner_gateway

    async def execute(self, command: GenerateScheduleCommand) -> GenerateScheduleResult:
        available_minutes = self._resolve_available_minutes(command)
        schedulable, unscheduled = self._split_tasks(command.tasks)

        ranked_tasks: list[RankedTask] = []
        planner_model: str | None = None
        used_fallback_ranking = False

        if schedulable:
            try:
                ranked_tasks, planner_model = await self._planner_gateway.rank_tasks(
                    tasks=schedulable,
                    user_state=command.user_state,
                    recent_executions=command.recent_executions or [],
                )
            except Exception:
                ranked_tasks = self._build_fallback_ranking(
                    schedulable,
                    command.user_state,
                    command.recent_executions or [],
                )
                planner_model = "deterministic-fallback"
                used_fallback_ranking = True
            else:
                if not ranked_tasks:
                    ranked_tasks = self._build_fallback_ranking(
                        schedulable,
                        command.user_state,
                        command.recent_executions or [],
                    )
                    planner_model = "deterministic-fallback"
                    used_fallback_ranking = True

        ordered_tasks = self._order_tasks(
            schedulable,
            ranked_tasks,
            command.user_state,
            command.recent_executions or [],
        )
        scheduled, extra_unscheduled = self._build_schedule(
            ordered_tasks=ordered_tasks,
            day_start_at=command.day_start_at,
            available_minutes=available_minutes,
        )
        unscheduled.extend(extra_unscheduled)
        explanations = self._build_explanations(
            user_state=command.user_state,
            recent_executions=command.recent_executions or [],
            scheduled=scheduled,
            unscheduled=unscheduled,
            used_fallback_ranking=used_fallback_ranking,
        )

        summary = ScheduleSummary(
            schedule_date=command.schedule_date,
            available_minutes=available_minutes,
            planned_minutes=sum(item.planned_minutes for item in scheduled),
            scheduled_count=len(scheduled),
            unscheduled_count=len(unscheduled),
            explanations=explanations,
            planner_model=planner_model,
            used_fallback_ranking=used_fallback_ranking,
        )

        return GenerateScheduleResult(
            schedule=GeneratedSchedule(
                scheduled=scheduled,
                unscheduled=unscheduled,
                summary=summary,
            )
        )

    def _resolve_available_minutes(self, command: GenerateScheduleCommand) -> int:
        if command.available_minutes is None and command.day_end_at is None:
            raise InvalidScheduleRequestError(
                "Either available_minutes or day_end_at must be provided"
            )

        if command.available_minutes is not None and command.available_minutes <= 0:
            raise InvalidScheduleRequestError("available_minutes must be greater than zero")

        if command.day_end_at is not None:
            if command.day_end_at <= command.day_start_at:
                raise InvalidScheduleRequestError("day_end_at must be after day_start_at")

            resolved_minutes = int(
                (command.day_end_at - command.day_start_at).total_seconds() // 60
            )
            if resolved_minutes <= 0:
                raise InvalidScheduleRequestError("Computed available minutes must be positive")

            if command.available_minutes is not None:
                return min(command.available_minutes, resolved_minutes)

            return resolved_minutes

        return command.available_minutes or 0

    def _split_tasks(
        self,
        tasks: list[PlanningTask],
    ) -> tuple[list[PlanningTask], list[UnscheduledTask]]:
        schedulable: list[PlanningTask] = []
        unscheduled: list[UnscheduledTask] = []

        for task in tasks:
            if task.status in INVALID_STATUSES:
                reason = (
                    UnscheduledTaskReason.COMPLETED
                    if task.status == PlanningTaskStatus.COMPLETED
                    else UnscheduledTaskReason.INVALID_STATUS
                )
                unscheduled.append(
                    UnscheduledTask(
                        task_id=task.id,
                        title=task.title,
                        remaining_minutes=task.remaining_minutes,
                        reason=reason,
                        reasoning="Task is not available for planning.",
                    )
                )
                continue

            if task.remaining_minutes <= 0:
                unscheduled.append(
                    UnscheduledTask(
                        task_id=task.id,
                        title=task.title,
                        remaining_minutes=0,
                        reason=UnscheduledTaskReason.NO_REMAINING_TIME,
                        reasoning="Task has no remaining time to plan.",
                    )
                )
                continue

            schedulable.append(task)

        return schedulable, unscheduled

    def _build_fallback_ranking(
        self,
        tasks: list[PlanningTask],
        user_state: UserStateSnapshot | None,
        recent_executions: list[RecentExecutionSnapshot],
    ) -> list[RankedTask]:
        recent_overload_penalty = 0.0
        if recent_executions:
            stressful_recent_count = sum(
                1
                for execution in recent_executions
                if execution.stress_after >= 7 or execution.energy_after <= 4
            )
            recent_overload_penalty = stressful_recent_count / len(recent_executions)

        def compute_rank_key(item: PlanningTask) -> tuple[float, datetime, int, int, str]:
            effort_penalty = 0.0

            if user_state is not None:
                low_energy = user_state.energy <= 4
                low_concentration = user_state.concentration <= 4
                low_motivation = user_state.motivation <= 4
                high_stress = user_state.stress >= 7
                poor_sleep = user_state.sleep_minutes < 6 * 60

                if low_energy or low_concentration or high_stress or poor_sleep:
                    effort_penalty += (
                        item.remaining_minutes * (item.complexity_level or 1)
                    ) / 10

                if low_motivation:
                    effort_penalty += item.remaining_minutes / 4

            if recent_overload_penalty > 0:
                effort_penalty += (
                    item.remaining_minutes
                    * (item.complexity_level or 1)
                    * recent_overload_penalty
                ) / 10

            return (
                -(item.priority * 100 - effort_penalty),
                item.deadline_at or datetime.max,
                item.remaining_minutes,
                item.complexity_level or 0,
                item.title.lower(),
            )

        sorted_tasks = sorted(
            tasks,
            key=compute_rank_key,
        )
        total = len(sorted_tasks)

        return [
            RankedTask(
                task_id=task.id,
                score=max(1, 100 - index * max(1, 100 // max(total, 1))),
                reasoning="Deterministic fallback ranking by priority, deadline and size.",
            )
            for index, task in enumerate(sorted_tasks)
        ]

    def _order_tasks(
        self,
        tasks: list[PlanningTask],
        ranked_tasks: list[RankedTask],
        user_state: UserStateSnapshot | None,
        recent_executions: list[RecentExecutionSnapshot],
    ) -> list[tuple[PlanningTask, RankedTask | None]]:
        tasks_by_id = {task.id: task for task in tasks}
        result: list[tuple[PlanningTask, RankedTask | None]] = []
        fallback_ranked = self._build_fallback_ranking(tasks, user_state, recent_executions)
        fallback_order = {
            item.task_id: index
            for index, item in enumerate(fallback_ranked)
        }

        for ranked in sorted(
            ranked_tasks,
            key=lambda item: (
                -item.score,
                fallback_order.get(item.task_id, 10**9),
                item.task_id,
            ),
        ):
            task = tasks_by_id.pop(ranked.task_id, None)
            if task is not None:
                result.append((task, ranked))

        for task in tasks_by_id.values():
            result.append((task, None))

        return result

    def _build_schedule(
        self,
        *,
        ordered_tasks: list[tuple[PlanningTask, RankedTask | None]],
        day_start_at: datetime,
        available_minutes: int,
    ) -> tuple[list[ScheduleBlock], list[UnscheduledTask]]:
        cursor = day_start_at
        remaining_window = available_minutes
        scheduled: list[ScheduleBlock] = []
        unscheduled: list[UnscheduledTask] = []

        for task, ranked in ordered_tasks:
            if remaining_window <= 0:
                unscheduled.append(
                    UnscheduledTask(
                        task_id=task.id,
                        title=task.title,
                        remaining_minutes=task.remaining_minutes,
                        reason=UnscheduledTaskReason.NOT_ENOUGH_TIME,
                        reasoning=ranked.reasoning if ranked else "No free time left in the day.",
                    )
                )
                continue

            planned_minutes = min(task.remaining_minutes, remaining_window)
            end_at = cursor + timedelta(minutes=planned_minutes)
            scheduled.append(
                ScheduleBlock(
                    task_id=task.id,
                    title=task.title,
                    start_at=cursor,
                    end_at=end_at,
                    planned_minutes=planned_minutes,
                    priority=task.priority,
                    reasoning=ranked.reasoning if ranked else None,
                )
            )
            cursor = end_at
            remaining_window -= planned_minutes

        return scheduled, unscheduled

    def _build_explanations(
        self,
        *,
        user_state: UserStateSnapshot | None,
        recent_executions: list[RecentExecutionSnapshot],
        scheduled: list[ScheduleBlock],
        unscheduled: list[UnscheduledTask],
        used_fallback_ranking: bool,
    ) -> list[str]:
        explanations: list[str] = []

        if used_fallback_ranking:
            explanations.append("План построен по приоритетам, дедлайнам и размеру задач.")

        if user_state is not None:
            if user_state.motivation <= 4:
                explanations.append("Из-за низкой мотивации выше поставлены более короткие задачи.")

            if (
                user_state.energy <= 4
                or user_state.concentration <= 4
                or user_state.stress >= 7
                or user_state.sleep_minutes < 6 * 60
            ):
                explanations.append("План сделан осторожнее из-за усталости или риска перегруза.")

        if recent_executions:
            stressful_recent_count = sum(
                1
                for execution in recent_executions
                if execution.stress_after >= 7 or execution.energy_after <= 4
            )
            if stressful_recent_count > 0:
                explanations.append("Учтены прошлые задачи, после которых вырос стресс или снизилась энергия.")

            overrun_count = sum(
                1
                for execution in recent_executions
                if execution.actual_minutes > execution.estimated_minutes
            )
            if overrun_count > 0:
                explanations.append("Учтен перерасход времени в прошлых выполнениях.")

        if unscheduled:
            explanations.append("Часть задач не попала в план, потому что не поместилась в доступное время.")

        if not explanations and scheduled:
            explanations.append("Задачи упорядочены по приоритету, дедлайну и оставшемуся времени.")

        return explanations[:3]
