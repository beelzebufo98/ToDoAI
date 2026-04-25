from __future__ import annotations

from enum import StrEnum
from typing import Any

import httpx

from app.config import settings


class OpenRouterModelPurpose(StrEnum):
    UX = "ux"
    PLANNING = "planning"


class OpenRouterConfigurationError(RuntimeError):
    pass


class OpenRouterRequestError(RuntimeError):
    pass


class OpenRouterClient:
    def __init__(self) -> None:
        if not settings.openrouter_is_configured:
            raise OpenRouterConfigurationError("OPENROUTER_API_KEY is not configured")

        headers = {
            "Authorization": f"Bearer {settings.openrouter_api_key}",
            "Content-Type": "application/json",
        }

        if settings.openrouter_http_referer:
            headers["HTTP-Referer"] = settings.openrouter_http_referer

        if settings.openrouter_app_title:
            headers["X-Title"] = settings.openrouter_app_title

        self._client = httpx.AsyncClient(
            base_url=settings.openrouter_base_url.rstrip("/"),
            headers=headers,
            timeout=settings.openrouter_timeout_seconds,
        )

    async def close(self) -> None:
        await self._client.aclose()

    async def __aenter__(self) -> "OpenRouterClient":
        return self

    async def __aexit__(self, exc_type: Any, exc: Any, tb: Any) -> None:
        await self.close()

    def get_model(self, purpose: OpenRouterModelPurpose) -> str:
        if purpose == OpenRouterModelPurpose.UX:
            return settings.openrouter_ux_model

        return settings.openrouter_planning_model

    async def chat_completion(
        self,
        *,
        messages: list[dict[str, Any]],
        purpose: OpenRouterModelPurpose,
        temperature: float = 0.2,
        max_tokens: int | None = None,
        response_format: dict[str, Any] | None = None,
    ) -> dict[str, Any]:
        payload: dict[str, Any] = {
            "model": self.get_model(purpose),
            "messages": messages,
            "temperature": temperature,
        }

        if max_tokens is not None:
            payload["max_tokens"] = max_tokens

        if response_format is not None:
            payload["response_format"] = response_format

        response = await self._client.post("/chat/completions", json=payload)

        try:
            response.raise_for_status()
        except httpx.HTTPStatusError as exc:
            details = response.text.strip()
            raise OpenRouterRequestError(
                f"OpenRouter request failed with status {response.status_code}: {details}"
            ) from exc

        data = response.json()
        choices = data.get("choices") or []
        if not choices:
            raise OpenRouterRequestError("OpenRouter returned no choices")

        message = choices[0].get("message") or {}
        content = message.get("content")
        if content is None:
            raise OpenRouterRequestError("OpenRouter returned empty message content")

        return {
            "model": payload["model"],
            "content": content,
            "raw": data,
        }
