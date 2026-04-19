from pydantic_settings import BaseSettings, SettingsConfigDict

from config.constants import ENV_FILE_PATH


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

    model_config = SettingsConfigDict(
        env_file=ENV_FILE_PATH,
        env_file_encoding="utf-8",
    )


settings = Settings()
