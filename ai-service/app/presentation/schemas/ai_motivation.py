from __future__ import annotations

from typing import Literal

from pydantic import BaseModel

MotivationTrigger = Literal["login", "task_completion"]


class GenerateMotivationRequest(BaseModel):
    trigger: MotivationTrigger


class GenerateMotivationResponse(BaseModel):
    message: str
    model: str
