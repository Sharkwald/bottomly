from commands.abstract_command import AbstractCommand
from model.member import Member


class AddMemberCommand(AbstractCommand):

    def get_purpose(self):
        return "Adds a new chat member to the persistent storage."

    def execute(self):
        # Check if username is unique, otherwise barf
        member_exists = len(list(Member.objects.raw({'_id': self.username}))) != 0
        if member_exists:
            return

        # instantiate & save new member
        m = Member(username=self.username, slack_id=self.slack_id)
        m.save()

    def __init__(self, username, slack_id):
        super(AddMemberCommand, self)
        self.username = username
        self.slack_id = slack_id
