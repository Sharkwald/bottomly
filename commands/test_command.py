from commands.abstract_command import AbstractCommand


class TestCommand(AbstractCommand):
    def get_purpose(self):
        return "Test's bottomly\'s connection to slack"

    def execute(self):
        return 'OK'

    def __init__(self):
        super(TestCommand, self)
