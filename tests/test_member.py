import unittest
from datetime import datetime, timedelta
from model.karma import Karma, KarmaType
from model.member import Member


def default_karma_list():
    """Returns a list of 3 karma entries, 2 negative, 1 positive"""
    return list([Karma(karma_type=KarmaType.NEGGYNEG), Karma(karma_type=KarmaType.NEGGYNEG),
                 Karma(karma_type=KarmaType.POZZYPOZ)])


class TestMember(unittest.TestCase):

    def test_get_current_karma_with_expired(self):
        # Arrange
        newly_awarded = datetime.today()
        award_ages_ago = datetime.today() - timedelta(days=31)
        new_karma = Karma(awarded=newly_awarded)
        old_karma = Karma(awarded=award_ages_ago)
        karma_list = list([new_karma, old_karma])
        m = Member(karma_list)

        # Act
        current_karma = m.get_current_karma()

        # Assert
        self.assertEqual(1, current_karma)

    def test_get_current_karma_with_net(self):
        # Arrange
        m = Member(default_karma_list())

        # Act
        net_karma = m.get_current_karma()

        # Assert
        self.assertEqual(-1, net_karma)

    def test_add_new_karma(self):
        # Arrange
        m = Member(default_karma_list())

        k = Karma(karma_type=KarmaType.POZZYPOZ)

        # Act
        m.add_karma(k)

        # Assert
        self.assertEqual(0, m.get_current_karma())

    def test_get_karma_reasons_all_default(self):
        # Arrange
        m = Member(default_karma_list())

        # Act
        reasons = m.get_karma_reasons()

        # Assert
        self.assertEqual(0, len(reasons))

    def test_get_karma_reasons_one_non_default(self):
        # Arrange
        karma_list = default_karma_list()
        karma_with_reason = Karma(reason="This is a silly reason")
        karma_list.append(karma_with_reason)
        m = Member(karma_list)

        # Act
        reasons = m.get_karma_reasons()

        # Assert
        self.assertEqual(list([karma_with_reason]), reasons)

if __name__ == '__main__':
    unittest.main()
