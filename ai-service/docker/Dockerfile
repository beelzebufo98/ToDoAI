FROM python:3.11-slim AS base

ENV PYTHONUNBUFFERED=1 \
    PIP_DISABLE_PIP_VERSION_CHECK=on \
    POETRY_VERSION=2.3.4 \
    POETRY_NO_INTERACTION=1 \
    POETRY_VIRTUALENVS_CREATE=false \
    PYSETUP_PATH=/opt/pysetup

WORKDIR ${PYSETUP_PATH}

FROM base AS builder

RUN apt-get update \
    && apt-get install --no-install-recommends -y build-essential curl \
    && pip install "poetry==${POETRY_VERSION}" \
    && rm -rf /var/lib/apt/lists/*

COPY poetry.lock pyproject.toml ./

RUN poetry install --without dev --no-root

FROM builder AS development

RUN poetry install --no-root

WORKDIR /app

CMD ["python", "app/main.py"]

FROM base AS production

COPY --from=builder /usr/local /usr/local

WORKDIR /app

COPY ./app /app/app
COPY pyproject.toml /app/pyproject.toml

CMD ["python", "app/main.py"]
