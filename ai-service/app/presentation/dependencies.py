from app.application import GenerateScheduleUseCase
from app.infrastructure import OpenRouterSchedulePlanner


def get_generate_schedule_use_case() -> GenerateScheduleUseCase:
    return GenerateScheduleUseCase(planner_gateway=OpenRouterSchedulePlanner())
