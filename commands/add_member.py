from model.member import Member


class AddMember(object):

    def execute(self):
        # Check if username is unique, otherwise barf

        # instantiate & save new member
        m = Member(self.username, [])
        m.save()

    def __init__(self, username):
        super(AddMember, self)
        self.username = username
