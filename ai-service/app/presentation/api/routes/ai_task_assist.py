from __future__ import annotations

import json
import logging

from fastapi import APIRouter, HTTPException, status

from app.presentation.schemas.ai_task_assist import (
    GenerateTaskAssistRequest,
    GenerateTaskAssistResponse,
)
from app.services import OpenRouterClient, OpenRouterModelPurpose

router = APIRouter(prefix="/ai/task-assist", tags=["AI Task Assist"])
logger = logging.getLogger(__name__)


@router.post(
    "",
    response_model=GenerateTaskAssistResponse,
    status_code=status.HTTP_200_OK,
)
async def generate_task_assist(
    request: GenerateTaskAssistRequest,
) -> GenerateTaskAssistResponse:
    logger.info("AI task assist request received. deadline_at=%s", request.deadline_at)

    prompt = json.dumps(
        {
            "taskDraft": {
                "title": request.title,
                "description": request.description,
                "deadlineAt": request.deadline_at.isoformat(),
            },
            "instructions": [
                "Improve the task title to be short, concrete and action-oriented.",
                "Rewrite the description so it is clearer and more structured, but keep the original intent.",
                "Estimate realistic effort in minutes for completing the task.",
                "Set complexityLevel as an integer from 1 to 10.",
                "Set priority as an integer from 1 to 10, taking the deadline into account.",
                "Return one short reasoning string in Russian that explains the estimate and priority.",
                "Do not use markdown.",
                "Return a single valid JSON object only.",
            ],
            "outputFormat": {
                "suggestedTitle": "string",
                "suggestedDescription": "string",
                "suggestedEstimatedMinutes": 90,
                "suggestedComplexityLevel": 6,
                "suggestedPriority": 7,
                "reasoning": "Короткое объяснение на русском.",
            },
        },
        ensure_ascii=True,
    )

    try:
        async with OpenRouterClient() as client:
            result = await client.chat_completion(
                messages=[
                    {
                        "role": "system",
                        "content": (
                            "You help users formulate productivity tasks. "
                            "Return a single valid JSON object only. "
                            "Be concrete, realistic and conservative with time estimates."
                        ),
                    },
                    {
                        "role": "user",
                        "content": prompt,
                    },
                ],
                purpose=OpenRouterModelPurpose.PLANNING,
                temperature=0.2,
                max_tokens=700,
                response_format={"type": "json_object"},
            )
    except Exception as exc:
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail=f"AI task assist failed: {exc}",
        ) from exc

    payload = _parse_json_payload(result["content"])

    suggested_title = _normalize_text(payload.get("suggestedTitle"))
    suggested_description = _normalize_text(payload.get("suggestedDescription"))
    reasoning = _normalize_text(payload.get("reasoning"))
    estimated_minutes = _to_int(payload.get("suggestedEstimatedMinutes"))
    complexity_level = _to_int(payload.get("suggestedComplexityLevel"))
    priority = _to_int(payload.get("suggestedPriority"))

    if not suggested_title:
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail="AI task assist response did not contain a valid suggestedTitle",
        )

    if not suggested_description:
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail="AI task assist response did not contain a valid suggestedDescription",
        )

    if not reasoning:
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail="AI task assist response did not contain a valid reasoning",
        )

    if estimated_minutes <= 0:
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail="AI task assist response did not contain a valid suggestedEstimatedMinutes",
        )

    if not 1 <= complexity_level <= 10:
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail="AI task assist response did not contain a valid suggestedComplexityLevel",
        )

    if not 1 <= priority <= 10:
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail="AI task assist response did not contain a valid suggestedPriority",
        )

    logger.info("AI task assist generated. model=%s", result["model"])

    return GenerateTaskAssistResponse(
        suggested_title=suggested_title[:120],
        suggested_description=suggested_description[:2000],
        suggested_estimated_minutes=estimated_minutes,
        suggested_complexity_level=complexity_level,
        suggested_priority=priority,
        reasoning=reasoning[:400],
    )


def _parse_json_payload(content: str) -> dict[str, object]:
    cleaned = content.strip()
    if cleaned.startswith("```"):
        cleaned = cleaned.split("\n", 1)[1]
        cleaned = cleaned.rsplit("```", 1)[0].strip()

    parsed = json.loads(cleaned)
    if not isinstance(parsed, dict):
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail="AI task assist response must be a JSON object",
        )

    return parsed


def _normalize_text(value: object) -> str:
    if not isinstance(value, str):
        return ""

    return " ".join(value.split()).strip()


def _to_int(value: object) -> int:
    try:
        return int(value)
    except (TypeError, ValueError):
        return 0
