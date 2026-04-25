from __future__ import annotations

from datetime import date, datetime
from typing import Literal

from pydantic import BaseModel, ConfigDict, Field, model_validator
from pydantic.alias_generators import to_camel


TaskStatus = Literal["new", "todo", "running", "completed", "deleted"]
UnscheduledReason = Literal[
    "completed",
    "no_remaining_time",
    "not_enough_time",
    "invalid_status",
]


class ApiSchema(BaseModel):
    model_config = ConfigDict(
        populate_by_name=True,
        alias_generator=to_camel,
    )


class UserStateRequest(ApiSchema):
    energy_level: int = Field(ge=1, le=10)
    stress_level: int = Field(ge=1, le=10)
    motivation_level: int = Field(ge=1, le=10)
    concentration_level: int = Field(ge=1, le=10)
    sleep_minutes: int = Field(ge=0)


class RecentExecutionRequest(ApiSchema):
    task_id: str = Field(min_length=1)
    task_title: str = Field(min_length=1)
    estimated_minutes: int = Field(gt=0)
    actual_minutes: int = Field(gt=0)
    priority: int = Field(ge=0)
    complexity_level: int = Field(ge=0)
    energy_after: int = Field(ge=1, le=10)
    stress_after: int = Field(ge=1, le=10)
    created_at: datetime


class PlanningTaskRequest(ApiSchema):
    id: str = Field(min_length=1)
    title: str = Field(min_length=1)
    estimated_minutes: int = Field(gt=0)
    remaining_minutes: int = Field(ge=0)
    priority: int = Field(ge=0)
    work_status: TaskStatus
    description: str | None = None
    deadline_at: datetime | None = None
    complexity_level: int | None = Field(default=None, ge=0)


class GenerateScheduleRequest(ApiSchema):
    schedule_date: date
    day_start_at: datetime
    available_minutes: int | None = Field(default=None, gt=0)
    day_end_at: datetime | None = None
    user_state: UserStateRequest | None = None
    recent_executions: list[RecentExecutionRequest] = Field(default_factory=list)
    tasks: list[PlanningTaskRequest] = Field(min_length=1)

    @model_validator(mode="after")
    def validate_window(self) -> "GenerateScheduleRequest":
        if self.available_minutes is None and self.day_end_at is None:
            raise ValueError("Either available_minutes or day_end_at must be provided")

        if self.day_end_at is not None and self.day_end_at <= self.day_start_at:
            raise ValueError("day_end_at must be after day_start_at")

        return self


class ScheduleBlockResponse(ApiSchema):
    task_id: str
    title: str
    start_at: datetime
    end_at: datetime
    planned_minutes: int
    priority: int
    reasoning: str | None = None


class UnscheduledTaskResponse(ApiSchema):
    task_id: str
    title: str
    remaining_minutes: int
    reason: UnscheduledReason
    reasoning: str | None = None


class ScheduleSummaryResponse(ApiSchema):
    schedule_date: date
    available_minutes: int
    planned_minutes: int
    scheduled_count: int
    unscheduled_count: int
    explanations: list[str] = Field(default_factory=list)
    planner_model: str | None = None
    used_fallback_ranking: bool
    generated_at: datetime


class GenerateScheduleResponse(ApiSchema):
    scheduled: list[ScheduleBlockResponse]
    unscheduled: list[UnscheduledTaskResponse]
    summary: ScheduleSummaryResponse
