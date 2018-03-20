from model.member import Member


class AddMemberCommand(object):

    def execute(self):
        # Check if username is unique, otherwise barf
        member_exists = len(list(Member.objects.raw({'_id': self.username}))) != 0
        if member_exists:
            return

        # instantiate & save new member
        m = Member(self.username)
        m.save()

    def __init__(self, username):
        super(AddMemberCommand, self)
        self.username = username
