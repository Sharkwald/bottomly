from commands.abstract_command import AbstractCommand


class VoidCommand(AbstractCommand):
    """
    An empty command that invokes as little logic as possible.
    This is intended to be used by test handlers, purely to ensure
    program execution gets down to the command layer.
    NB: This could potentially change in future if a use case around
    a handler which executes a void command could be described.
    """

    def get_purpose(self):
        return "Tests execution flow as far as the command layer."

    def execute(self):
        return ""

    def __init__(self):
        super(VoidCommand, self)
