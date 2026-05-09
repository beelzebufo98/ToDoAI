from __future__ import annotations

from datetime import datetime

from pydantic import BaseModel, ConfigDict, Field
from pydantic.alias_generators import to_camel


class ApiSchema(BaseModel):
    model_config = ConfigDict(
        populate_by_name=True,
        alias_generator=to_camel,
    )


class GenerateTaskAssistRequest(ApiSchema):
    title: str = Field(min_length=6, max_length=120)
    description: str = Field(min_length=20, max_length=2000)
    deadline_at: datetime


class GenerateTaskAssistResponse(ApiSchema):
    suggested_title: str
    suggested_description: str
    suggested_estimated_minutes: int = Field(gt=0)
    suggested_complexity_level: int = Field(ge=1, le=10)
    suggested_priority: int = Field(ge=1, le=10)
    reasoning: str
