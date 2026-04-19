from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from config import settings


def _init_router(_app: FastAPI) -> None:
    from api import router

    _app.include_router(router)


def _init_middleware(_app: FastAPI) -> None:
    _app.add_middleware(
        CORSMiddleware,
        allow_origins=settings.cors_origins,
        allow_credentials=settings.cors_allow_credentials,
        allow_methods=settings.cors_allow_methods,
        allow_headers=settings.cors_allow_headers,
    )

def create_app() -> FastAPI:
    _app = FastAPI(
        title="ToDoAI Planning Service",
        description="AI service for day schedule planning",
        version="1.0.0",
        docs_url=settings.docs_url,
        redoc_url=settings.redoc_url,
    )
    _init_middleware(_app)
    _init_router(_app)
    return _app

app = create_app()
