from __future__ import annotations

import json
from typing import Any

from app.application.ports import PlanningModelGateway, RankedTask
from app.domain.schedule import PlanningTask, RecentExecutionSnapshot, UserStateSnapshot
from app.services import OpenRouterClient, OpenRouterModelPurpose


class OpenRouterSchedulePlanner(PlanningModelGateway):
    async def rank_tasks(
        self,
        *,
        tasks: list[PlanningTask],
        user_state: UserStateSnapshot | None,
        recent_executions: list[RecentExecutionSnapshot],
    ) -> tuple[list[RankedTask], str]:
        prompt = self._build_prompt(
            tasks=tasks,
            user_state=user_state,
            recent_executions=recent_executions,
        )

        async with OpenRouterClient() as client:
            result = await client.chat_completion(
                messages=[
                    {
                        "role": "system",
                        "content": (
                            "You are an expert day-planning assistant. "
                            "You rank tasks for a single day using the user's current state. "
                            "Be conservative under fatigue or overload. "
                            "Strongly penalize long, hard tasks when energy or concentration is low, "
                            "sleep is poor, or stress is high. "
                            "Prefer quick wins and shorter tasks when motivation is low. "
                            "Urgent deadlines and very high priority still matter, but do not overload the user. "
                            "Return a single valid JSON object only."
                        ),
                    },
                    {
                        "role": "user",
                        "content": prompt,
                    },
                ],
                purpose=OpenRouterModelPurpose.PLANNING,
                temperature=0,
                max_tokens=1200,
                response_format={"type": "json_object"},
            )

        parsed = self._parse_json_payload(result["content"])
        ranked_tasks = [
            RankedTask(
                task_id=item["taskId"],
                score=int(item["score"]),
                reasoning=item.get("reason"),
            )
            for item in parsed.get("rankedTasks", [])
            if item.get("taskId")
        ]
        return ranked_tasks, result["model"]

    def _build_prompt(
        self,
        *,
        tasks: list[PlanningTask],
        user_state: UserStateSnapshot | None,
        recent_executions: list[RecentExecutionSnapshot],
    ) -> str:
        tasks_payload = [
            {
                "taskId": task.id,
                "title": task.title,
                "description": task.description,
                "priority": task.priority,
                "complexityLevel": task.complexity_level,
                "status": task.status.value,
                "estimatedMinutes": task.estimated_minutes,
                "remainingMinutes": task.remaining_minutes,
                "deadlineAt": task.deadline_at.isoformat() if task.deadline_at else None,
            }
            for task in tasks
        ]

        payload: dict[str, Any] = {
            "userState": (
                {
                    "energyLevel": user_state.energy,
                    "stressLevel": user_state.stress,
                    "motivationLevel": user_state.motivation,
                    "concentrationLevel": user_state.concentration,
                    "sleepMinutes": user_state.sleep_minutes,
                }
                if user_state is not None
                else None
            ),
            "recentExecutions": [
                {
                    "taskId": execution.task_id,
                    "taskTitle": execution.task_title,
                    "estimatedMinutes": execution.estimated_minutes,
                    "actualMinutes": execution.actual_minutes,
                    "priority": execution.priority,
                    "complexityLevel": execution.complexity_level,
                    "energyAfter": execution.energy_after,
                    "stressAfter": execution.stress_after,
                    "createdAt": execution.created_at.isoformat(),
                }
                for execution in recent_executions
            ],
            "tasks": tasks_payload,
            "instructions": [
                "Rank the tasks in the best execution order for a single day.",
                "Prefer urgent and high priority tasks, but not at the cost of obvious overload.",
                "Consider user energy, stress, concentration, motivation and sleep strongly.",
                "Use recentExecutions as feedback about how previous completed tasks affected the user.",
                "If recentExecutions show low energy after work or high stress after work, be more conservative.",
                "If recentExecutions show large overruns on hard tasks, be more careful with long difficult tasks today.",
                "Penalize long and hard tasks when energy is low, concentration is low, sleep is poor, or stress is high.",
                "Prefer quick wins, shorter tasks and lower-friction tasks when motivation is low.",
                "If the user state indicates fatigue or overload, favor tasks with lower complexity and shorter remaining time.",
                "Return every input task exactly once.",
                "Score must be an integer from 1 to 100.",
                "Reason must be short and concrete.",
                "Do not invent task ids or omit tasks.",
            ],
            "outputFormat": {
                "rankedTasks": [
                    {
                        "taskId": "string",
                        "score": 100,
                        "reason": "short explanation",
                    }
                ]
            },
        }

        return json.dumps(payload, ensure_ascii=True)

    def _parse_json_payload(self, content: str) -> dict[str, Any]:
        cleaned = content.strip()
        if cleaned.startswith("```"):
            cleaned = cleaned.split("\n", 1)[1]
            cleaned = cleaned.rsplit("```", 1)[0].strip()

        parsed = json.loads(cleaned)
        if not isinstance(parsed, dict):
            raise ValueError("Planner response must be a JSON object")

        ranked_tasks = parsed.get("rankedTasks")
        if not isinstance(ranked_tasks, list):
            raise ValueError("Planner response must contain rankedTasks array")

        return parsed
