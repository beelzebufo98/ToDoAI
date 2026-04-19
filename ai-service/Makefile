.PHONY: help run test lint

help:
	@echo "Available commands:"
	@echo "  make run   - start the FastAPI service"
	@echo "  make test  - run tests"
	@echo "  make lint  - run ruff"

run:
	python app/main.py

test:
	pytest

lint:
	ruff check app tests
