import unittest
from datetime import datetime

from config import Config
from model.karma import Karma, KarmaType


class TestKarma(unittest.TestCase):
    def test_persistence(self):
        # Arrange
        Config().connect_to_db()
        awarded_to = "testUser1"
        awarded_by = "testUser2"
        test_reason = "testReason"
        test_awarded = datetime.today()
        test_type = KarmaType.POZZYPOZ
        k = Karma(awarded_to_username=awarded_to,
                  reason=test_reason,
                  awarded_by_username=awarded_by,
                  awarded=test_awarded,
                  karma_type=test_type)

        # Act
        k.save()

        # Assert
        loaded_karma = Karma.objects.all()[0]
        self.assertEqual(k, loaded_karma)

        # Tear down
        k.delete()

if __name__ == '__main__':
    unittest.main()
