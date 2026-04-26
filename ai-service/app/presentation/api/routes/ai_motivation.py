from __future__ import annotations

import json
import logging

from fastapi import APIRouter, HTTPException, status

from app.presentation.schemas.ai_motivation import (
    GenerateMotivationRequest,
    GenerateMotivationResponse,
)
from app.services import OpenRouterClient, OpenRouterModelPurpose

router = APIRouter(prefix="/ai/motivation", tags=["AI Motivation"])
logger = logging.getLogger(__name__)


@router.post(
    "/generate",
    response_model=GenerateMotivationResponse,
    status_code=status.HTTP_200_OK,
)
async def generate_motivation(
    request: GenerateMotivationRequest,
) -> GenerateMotivationResponse:
    logger.info("AI motivation request received. trigger=%s", request.trigger)

    prompt = json.dumps(
        {
            "trigger": request.trigger,
            "instructions": [
                "Write one short motivational message in Russian.",
                "Maximum length: 120 characters.",
                "No markdown, no emojis, no lists, no quotes.",
                "Keep the tone warm, simple and grounded.",
                "If trigger is login, welcome the user and encourage starting the day.",
                "If trigger is task_completion, acknowledge progress and encourage continuing.",
            ],
            "outputFormat": {
                "message": "short motivational message in Russian"
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
                            "You generate short supportive product messages for a productivity app. "
                            "Return a single valid JSON object only."
                        ),
                    },
                    {
                        "role": "user",
                        "content": prompt,
                    },
                ],
                purpose=OpenRouterModelPurpose.UX,
                temperature=0.8,
                max_tokens=120,
                response_format={"type": "json_object"},
            )
    except Exception as exc:
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail=f"AI motivation failed: {exc}",
        ) from exc

    payload = _parse_json_payload(result["content"])
    message = payload.get("message")
    if not isinstance(message, str) or not message.strip():
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail="AI motivation response did not contain a valid message",
        )

    cleaned_message = " ".join(message.split()).strip()

    logger.info("AI motivation generated. trigger=%s model=%s", request.trigger, result["model"])

    return GenerateMotivationResponse(
        message=cleaned_message[:120],
        model=result["model"],
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
            detail="AI motivation response must be a JSON object",
        )

    return parsed
