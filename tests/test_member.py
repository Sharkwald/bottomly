import unittest
from datetime import datetime, timedelta
from model.karma import Karma
from model.karma import KarmaType
from model.member import Member


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
        karma_list = list([Karma(karma_type=KarmaType.NEGGYNEG), Karma(karma_type=KarmaType.NEGGYNEG),
                           Karma(karma_type=KarmaType.POZZYPOZ)])
        m = Member(karma_list)

        # Act
        net_karma = m.get_current_karma()

        # Assert
        self.assertEqual(-1, net_karma)


if __name__ == '__main__':
    unittest.main()
