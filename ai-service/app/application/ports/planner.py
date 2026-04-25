from __future__ import annotations

from dataclasses import dataclass
from typing import Protocol

from app.domain.schedule import PlanningTask, RecentExecutionSnapshot, UserStateSnapshot


@dataclass(slots=True, frozen=True)
class RankedTask:
    task_id: str
    score: int
    reasoning: str | None = None


class PlanningModelGateway(Protocol):
    async def rank_tasks(
        self,
        *,
        tasks: list[PlanningTask],
        user_state: UserStateSnapshot | None,
        recent_executions: list[RecentExecutionSnapshot],
    ) -> tuple[list[RankedTask], str]:
        raise NotImplementedError
