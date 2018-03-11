import unittest
from datetime import datetime, timedelta
from model.karma import Karma
from model.member import Member


class TestMember(unittest.TestCase):
    def test_get_current_karma(self):
        newly_awarded = datetime.today()
        award_ages_ago = datetime.today() - timedelta(days=31)
        new_karma = Karma(awarded=newly_awarded)
        old_karma = Karma(awarded=award_ages_ago)
        karmas = list([new_karma, old_karma])

        m = Member(karmas)

        current_karma = m.get_current_karma()

        self.assertEqual(current_karma, 1)


if __name__ == '__main__':
    unittest.main()
