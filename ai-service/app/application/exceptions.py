class ApplicationError(RuntimeError):
    pass


class InvalidScheduleRequestError(ApplicationError):
    pass
