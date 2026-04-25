from __future__ import annotations

from dataclasses import dataclass, field
from datetime import date, datetime
from enum import StrEnum


class PlanningTaskStatus(StrEnum):
    NEW = "new"
    TODO = "todo"
    RUNNING = "running"
    COMPLETED = "completed"
    DELETED = "deleted"


class UnscheduledTaskReason(StrEnum):
    COMPLETED = "completed"
    NO_REMAINING_TIME = "no_remaining_time"
    NOT_ENOUGH_TIME = "not_enough_time"
    INVALID_STATUS = "invalid_status"


@dataclass(slots=True, frozen=True)
class UserStateSnapshot:
    energy: int
    stress: int
    motivation: int
    concentration: int
    sleep_minutes: int


@dataclass(slots=True, frozen=True)
class RecentExecutionSnapshot:
    task_id: str
    task_title: str
    estimated_minutes: int
    actual_minutes: int
    priority: int
    complexity_level: int
    energy_after: int
    stress_after: int
    created_at: datetime


@dataclass(slots=True, frozen=True)
class PlanningTask:
    id: str
    title: str
    estimated_minutes: int
    remaining_minutes: int
    priority: int
    status: PlanningTaskStatus
    description: str | None = None
    deadline_at: datetime | None = None
    complexity_level: int | None = None


@dataclass(slots=True, frozen=True)
class ScheduleBlock:
    task_id: str
    title: str
    start_at: datetime
    end_at: datetime
    planned_minutes: int
    priority: int
    reasoning: str | None = None


@dataclass(slots=True, frozen=True)
class UnscheduledTask:
    task_id: str
    title: str
    remaining_minutes: int
    reason: UnscheduledTaskReason
    reasoning: str | None = None


@dataclass(slots=True, frozen=True)
class ScheduleSummary:
    schedule_date: date
    available_minutes: int
    planned_minutes: int
    scheduled_count: int
    unscheduled_count: int
    explanations: list[str] = field(default_factory=list)
    planner_model: str | None = None
    used_fallback_ranking: bool = False


@dataclass(slots=True, frozen=True)
class GeneratedSchedule:
    scheduled: list[ScheduleBlock] = field(default_factory=list)
    unscheduled: list[UnscheduledTask] = field(default_factory=list)
    summary: ScheduleSummary | None = None
