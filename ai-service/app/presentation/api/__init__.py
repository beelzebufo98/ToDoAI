from fastapi import APIRouter

from app.presentation.api.routes.ai_motivation import router as ai_motivation_router
from app.presentation.api.routes.ai_schedule import router as ai_schedule_router
from app.presentation.api.routes.ai_task_assist import router as ai_task_assist_router

router = APIRouter(prefix="/api/v1")
router.include_router(ai_schedule_router)
router.include_router(ai_motivation_router)
router.include_router(ai_task_assist_router)

__all__ = ["router"]
