# coding=utf-8
from datetime import datetime

from commands.abstract_command import AbstractCommand
from config import Config
from model.karma import Karma, KarmaType


class AddKarmaCommand(AbstractCommand):

    def get_purpose(self):
        return "Awards an imaginary internet point to someone/something."

    def execute(self, awarded_to, awarded_by, reason, karma_type):
        self.config.connect_to_db()
        k = Karma(awarded_to_username=awarded_to,
                  reason=reason,
                  awarded_by_username=awarded_by,
                  awarded=datetime.now(),
                  karma_type=str(karma_type))

        k.save()
        return k

    @staticmethod
    def get_karma_reactions() -> dict:
        return {
            "+1": KarmaType.POZZYPOZ,
            "arrow_up": KarmaType.POZZYPOZ,
            "clap": KarmaType.POZZYPOZ,
            "heart": KarmaType.POZZYPOZ,
            "heart_eyes": KarmaType.POZZYPOZ,
            "heavy_plus_sign": KarmaType.POZZYPOZ,
            "heavy_tick": KarmaType.POZZYPOZ,
            "joy": KarmaType.POZZYPOZ,
            "party_parrot": KarmaType.POZZYPOZ,
            "raised_hands": KarmaType.POZZYPOZ,
            "smile": KarmaType.POZZYPOZ,
            "thumbsup": KarmaType.POZZYPOZ,
            "-1": KarmaType.NEGGYNEG,
            "arrow_down": KarmaType.NEGGYNEG,
            "hankey": KarmaType.NEGGYNEG,
            "heavy_minus_sign": KarmaType.NEGGYNEG,
            "poo": KarmaType.NEGGYNEG,
            "poop": KarmaType.NEGGYNEG,
            "shit": KarmaType.NEGGYNEG,
            "thumbsdown": KarmaType.NEGGYNEG
        }

    def __init__(self):
        super(AddKarmaCommand, self)
        self.config = Config()
