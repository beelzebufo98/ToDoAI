from pydantic_settings import BaseSettings, SettingsConfigDict

from app.config.constants import ENV_FILE_PATH


class Settings(BaseSettings):
    env: str = "development"
    app_host: str = "0.0.0.0"
    app_port: int = 8000
    workers: int = 1
    reload: bool = True
    docs_url: str | None = "/docs"
    redoc_url: str | None = "/redoc"
    cors_origins: list[str] = ["*"]
    cors_allow_credentials: bool = True
    cors_allow_methods: list[str] = ["*"]
    cors_allow_headers: list[str] = ["*"]
    openrouter_api_key: str = ""
    openrouter_base_url: str = "https://openrouter.ai/api/v1"
    openrouter_ux_model: str = "google/gemini-2.5-flash-lite"
    openrouter_planning_model: str = "google/gemini-2.5-flash"
    openrouter_timeout_seconds: float = 60.0
    openrouter_http_referer: str | None = None
    openrouter_app_title: str | None = "ToDoAI"

    @property
    def openrouter_is_configured(self) -> bool:
        return bool(self.openrouter_api_key.strip())

    model_config = SettingsConfigDict(
        env_file=ENV_FILE_PATH,
        env_file_encoding="utf-8",
        extra="ignore",
    )


settings = Settings()
