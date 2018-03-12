import unittest
from datetime import datetime
from model.member import Karma


class TestKarma(unittest.TestCase):
    def test_instantiate_karma(self):
        test_user = "testUser"
        test_reason = "testReason"
        test_awarded = datetime.today()
        k = Karma(test_user, test_reason, test_awarded)

        self.assertEqual(k.awarded_by_username, test_user)
        self.assertEqual(k.awarded, test_awarded)
        self.assertEqual(k.reason, test_reason)


if __name__ == '__main__':
    unittest.main()
