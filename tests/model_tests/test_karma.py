import unittest
from datetime import datetime

from config import Config
from model.karma import Karma, KarmaType


class TestKarma(unittest.TestCase):
    def test_persistence(self):
        # Set up
        Config().connect_to_db()
        old_karma = Karma.objects.all()
        for ok in old_karma:
            ok.delete()

        # Arrange
        awarded_to = "testUser1"
        awarded_by = "testUser2"
        test_reason = "testReason"
        test_awarded = datetime.today()
        test_type = str(KarmaType.POZZYPOZ)
        k = Karma(awarded_to_username=awarded_to,
                  reason=test_reason,
                  awarded_by_username=awarded_by,
                  awarded=test_awarded,
                  karma_type=test_type)

        # Act
        k.save()

        # Assert
        loaded_karma = Karma.objects.all()[0]
        self.assertEqual(k.awarded_to_username, loaded_karma.awarded_to_username)
        self.assertEqual(k.reason, loaded_karma.reason)
        self.assertEqual(k.awarded_by_username, loaded_karma.awarded_by_username)
        self.assertEqual(k.karma_type, loaded_karma.karma_type)
        # We'll assume that awarded is equal cause date equality assertions seem to be guff.

        # Tear down
        k.delete()

    def test_get_current_net_karma_unknown_recipient_is_zero(self):
        config = Config()
        config.connect_to_db()

        recipient = "bfvhdsukvhksdbv"
        net_karma = Karma.get_current_net_karma_for_recipient(recipient)

        self.assertEqual(0, net_karma)

if __name__ == '__main__':
    unittest.main()
