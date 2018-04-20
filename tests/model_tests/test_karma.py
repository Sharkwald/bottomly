import unittest
from datetime import datetime, timedelta

from config import Config
from model.karma import Karma, KarmaType

test_awarder = "default awarder"
test_recipient = "default recipient"


def create_karma(awarded_by_username=test_awarder,
                 awarded_to_username=test_recipient,
                 reason=Karma.default_reason,
                 awarded=datetime.today(),
                 karma_type=KarmaType.POZZYPOZ):
    k = Karma()
    k.awarded_by_username = awarded_by_username
    k.awarded_to_username = awarded_to_username
    k.reason = reason
    k.awarded = awarded
    k.karma_type = karma_type
    return k


def default_karma_list():
    """Returns a list of 4 karma entries, 2 negative, 2 positive"""
    return list([create_karma(karma_type=KarmaType.NEGGYNEG), create_karma(karma_type=KarmaType.NEGGYNEG),
                 create_karma(karma_type=KarmaType.POZZYPOZ), create_karma(karma_type=KarmaType.POZZYPOZ)])


class TestKarma(unittest.TestCase):
    @staticmethod
    def setup_leaderboard():
        newly_awarded = datetime.today()
        recently_awarded = datetime.today() - timedelta(days=5)
        award_ages_ago = datetime.today() - timedelta(days=31)
        cool_guy = "cool guy"
        guy_1 = "guy 1"
        guy_2 = "guy 2"
        loser = "loser"
        karma_leader_board = list([create_karma(awarded=newly_awarded, awarded_to_username=cool_guy),
                                   create_karma(awarded=recently_awarded, awarded_to_username=cool_guy.upper()),
                                   create_karma(awarded=recently_awarded, awarded_to_username=guy_1),
                                   create_karma(awarded=recently_awarded, awarded_to_username=guy_2),
                                   create_karma(awarded=recently_awarded, awarded_to_username=loser,
                                                karma_type=KarmaType.NEGGYNEG),
                                   create_karma(awarded=award_ages_ago)])
        for k in karma_leader_board:
            k.save()

        return karma_leader_board

    def setUp(self):
        # Set up
        Config().connect_to_db()
        old_karma = Karma.objects.all()
        for ok in old_karma:
            ok.delete()

    def test_persistence(self):
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

    def test_get_current_net_karma_unknown_recipient_is_zero(self):
        config = Config()
        config.connect_to_db()

        recipient = "bfvhdsukvhksdbv"
        net_karma = Karma.get_current_net_karma_for_recipient(recipient)

        self.assertEqual(0, net_karma)

    def test_get_leader_board(self):
        # Arrange
        self.setup_leaderboard()

        expected = [{"username": "cool guy", "net_karma": 2},
                    {"username": "guy 1", "net_karma": 1},
                    {"username": "guy 2", "net_karma": 1}]

        # Act
        leader_board = Karma.get_leader_board()

        # Assert
        self.assertEqual(expected, leader_board)

    def test_get_loser_board(self):
        # Arrange
        self.setup_leaderboard()

        expected = [{"username": "loser", "net_karma": -1},
                    {"username": "guy 1", "net_karma": 1},
                    {"username": "guy 2", "net_karma": 1}]

        # Act
        loser_board = Karma.get_loser_board()

        # Assert
        self.assertEqual(expected, loser_board)

    def test_get_current_karma_with_expired(self):
        # Arrange
        newly_awarded = datetime.today()
        award_ages_ago = datetime.today() - timedelta(days=31)
        new_karma = create_karma(awarded=newly_awarded)
        old_karma = create_karma(awarded=award_ages_ago)
        new_karma.save()
        old_karma.save()

        # Act
        current_karma = Karma.get_current_net_karma_for_recipient(test_recipient)

        # Assert
        self.assertEqual(1, current_karma)

    def test_get_current_karma_with_net(self):
        # Arrange
        karma_to_save = default_karma_list()
        for k in karma_to_save:
            k.save()

        # Act
        net_karma = Karma.get_current_net_karma_for_recipient(test_recipient)

        # Assert
        self.assertEqual(0, net_karma)

    def test_get_karma_reasons_all_default(self):
        # Arrange
        for k in default_karma_list():
            k.save()

        # Act
        karma_reasons = Karma.get_current_karma_reasons_for_recipient(test_recipient)

        # Assert
        self.assertEqual(len(default_karma_list()), karma_reasons['reasonless'])
        self.assertEqual(0, len(karma_reasons['reasoned']))

    def test_get_karma_reasons_one_non_default(self):
        # Arrange
        karma_list = default_karma_list()
        karma_with_reason = create_karma(reason="This is a silly reason")
        karma_list.append(karma_with_reason)

        for k in karma_list:
            k.save()

        # Act
        karma_reasons = Karma.get_current_karma_reasons_for_recipient(test_recipient)

        # Assert
        self.assertEqual(len(default_karma_list()), karma_reasons['reasonless'])
        self.assertEqual(list([karma_with_reason]), karma_reasons['reasoned'])

    def test_positive_karma_cannot_be_self_awarded(self):
        awarded_to = "testUser1"
        awarded_by = "testUser1"
        test_reason = "testReason"
        test_awarded = datetime.today()
        test_type = str(KarmaType.POZZYPOZ)
        k = Karma(awarded_to_username=awarded_to,
                  reason=test_reason,
                  awarded_by_username=awarded_by,
                  awarded=test_awarded,
                  karma_type=test_type)
        valid = k.validate()

        self.assertFalse(valid)

    def test_positive_karma_can_be_awarded_by_others(self):
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
        valid = k.validate()

        self.assertTrue(valid)

    def test_negative_karma_can_be_self_awarded(self):
        awarded_to = "testUser1"
        awarded_by = "testUser1"
        test_reason = "testReason"
        test_awarded = datetime.today()
        test_type = str(KarmaType.NEGGYNEG)
        k = Karma(awarded_to_username=awarded_to,
                  reason=test_reason,
                  awarded_by_username=awarded_by,
                  awarded=test_awarded,
                  karma_type=test_type)
        valid = k.validate()

        self.assertTrue(valid)

    def test_negative_karma_can_be_awarded_by_others(self):
        awarded_to = "testUser1"
        awarded_by = "testUser2"
        test_reason = "testReason"
        test_awarded = datetime.today()
        test_type = str(KarmaType.NEGGYNEG)
        k = Karma(awarded_to_username=awarded_to,
                  reason=test_reason,
                  awarded_by_username=awarded_by,
                  awarded=test_awarded,
                  karma_type=test_type)
        valid = k.validate()

        self.assertTrue(valid)

    def test_karma_calculation_bug(self):
        toms_karma = list([Karma(awarded_to_username="TomAllen", reason="", awarded_by_username="gee",
                                 awarded=datetime.now(), karma_type=KarmaType.NEGGYNEG),
                           Karma(awarded_to_username="TomAllen", reason="", awarded_by_username="gee",
                                 awarded=datetime.now(), karma_type=KarmaType.NEGGYNEG),
                           Karma(awarded_to_username="TomAllen", reason="", awarded_by_username="gee",
                                 awarded=datetime.now(), karma_type=KarmaType.NEGGYNEG)])

        for k in toms_karma:
            k.save()

        net_karma = Karma.get_current_net_karma_for_recipient("tomallen")

        self.assertEqual(-3, net_karma)

    def test_karma_reasons_do_not_filter_on_substrings(self):
        # Arrange
        dan_karma = Karma(awarded_to_username="dan", reason="a reason", awarded_by_username=test_awarder,
                          awarded=datetime.now(), karma_type=KarmaType.POZZYPOZ)
        dan_karma.save()

        seniordaniel_karma = Karma(awarded_to_username="seniordaniel", reason="a different reason",
                                   awarded_by_username=test_awarder, awarded=datetime.now(),
                                   karma_type=KarmaType.POZZYPOZ)
        seniordaniel_karma.save()

        expected_reasoned_karma = list([dan_karma])

        # Act
        dan_reasons = Karma.get_current_karma_reasons_for_recipient("dan")

        # Assert
        self.assertEqual(expected_reasoned_karma, dan_reasons["reasoned"])


if __name__ == '__main__':
    unittest.main()
