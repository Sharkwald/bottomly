import unittest
from model.member import Member
from model.karma import Karma, KarmaType
from config import Config
from unittest.mock import patch


class TestMember(unittest.TestCase):
    test_username = "Persistence test user"

    def test_persistence(self):
        # Arrange
        Config().connect_to_db()
        m = Member(TestMember.test_username)

        # Act
        m.save()

        # Assert
        retrieved_members = Member.objects.raw({'_id':TestMember.test_username})
        self.assertEqual(1, retrieved_members.count())
        self.assertEqual(m, retrieved_members[0])

        # Tear down
        m.delete()

    def test_get_current_karma(self):
        with patch.object(Karma, "get_current_net_karma_for_recipient") as test_method:
            m = Member(TestMember.test_username)

            m.get_current_karma()

            test_method.assert_called_once_with(TestMember.test_username)


    def test_get_karma_reasons(self):
        with patch.object(Karma, "get_current_karma_reasons_for_recipient") as test_method:
            m = Member(TestMember.test_username)

            m.get_karma_reasons()

            test_method.assert_called_once_with(TestMember.test_username)


if __name__ == '__main__':
    unittest.main()
