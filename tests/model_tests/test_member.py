import unittest
from datetime import datetime, timedelta
from model.member import Member
from model.karma import Karma, KarmaType
from config import Config

def create_karma(awarded_by_username="default awarder",
                 reason=Karma.default_reason,
                 awarded=datetime.today(),
                 karma_type=KarmaType.POZZYPOZ):
    k = Karma()
    k.awarded_by_username = awarded_by_username
    k.reason = reason
    k.awarded = awarded
    k.karma_type = karma_type
    return k

def default_karma_list():
    """Returns a list of 4 karma entries, 2 negative, 2 positive"""
    return list([create_karma(karma_type=KarmaType.NEGGYNEG), create_karma(karma_type=KarmaType.NEGGYNEG),
                 create_karma(karma_type=KarmaType.POZZYPOZ), create_karma(karma_type=KarmaType.POZZYPOZ)])


class TestMember(unittest.TestCase):
    test_username = "Persistence test user"

    def test_get_current_karma_with_expired(self):
        # Arrange
        newly_awarded = datetime.today()
        award_ages_ago = datetime.today() - timedelta(days=31)
        new_karma = create_karma(awarded=newly_awarded)
        old_karma = create_karma(awarded=award_ages_ago)
        karma_list = list([new_karma, old_karma])
        m = Member(TestMember.test_username, karma_list)

        # Act
        current_karma = m.get_current_karma()

        # Assert
        self.assertEqual(1, current_karma)

    def test_get_current_karma_with_net(self):
        # Arrange
        m = Member(TestMember.test_username, default_karma_list())

        # Act
        net_karma = m.get_current_karma()

        # Assert
        self.assertEqual(0, net_karma)

    def test_add_new_karma(self):
        # Arrange
        m = Member(TestMember.test_username, default_karma_list())
        k = create_karma(karma_type=KarmaType.POZZYPOZ)

        # Act
        m.add_karma(k)

        # Assert
        self.assertEqual(1, m.get_current_karma())

    def test_get_karma_reasons_all_default(self):
        # Arrange
        m = Member(TestMember.test_username, default_karma_list())

        # Act
        karma_reasons = m.get_karma_reasons()

        # Assert
        self.assertEqual(len(default_karma_list()), karma_reasons['reasonless'])
        self.assertEqual(0, len(karma_reasons['reasoned']))

    def test_get_karma_reasons_one_non_default(self):
        # Arrange
        karma_list = default_karma_list()
        karma_with_reason = create_karma(reason="This is a silly reason")
        karma_list.append(karma_with_reason)
        m = Member(TestMember.test_username, karma_list)

        # Act
        karma_reasons = m.get_karma_reasons()

        # Assert
        self.assertEqual(len(default_karma_list()), karma_reasons['reasonless'])
        self.assertEqual(list([karma_with_reason]), karma_reasons['reasoned'])

    def test_persistence(self):
        # Arrange
        Config().connect_to_db()
        m = Member(TestMember.test_username, default_karma_list())

        # Act
        m.save()

        # Assert
        retrieved_members = Member.objects.raw({'_id':TestMember.test_username})
        self.assertEqual(1, retrieved_members.count())
        self.assertEqual(m, retrieved_members[0])

        # Tear down
        m.delete()


if __name__ == '__main__':
    unittest.main()
