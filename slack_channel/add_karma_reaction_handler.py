import logging
from model.karma import KarmaType

from commands import AddKarmaCommand
from slack_channel.abstract_reaction_handler import AbstractReactionHandler

_karma_reactions = {
    "joy": KarmaType.POZZYPOZ,
    "+1": KarmaType.POZZYPOZ,
    "thumbsup": KarmaType.POZZYPOZ,
    "clap": KarmaType.POZZYPOZ,
    "arrow_up": KarmaType.POZZYPOZ,
    "heart": KarmaType.POZZYPOZ,
    "heart_eyes": KarmaType.POZZYPOZ,
    "smile": KarmaType.POZZYPOZ,
    "-1": KarmaType.NEGGYNEG,
    "thumbsdown": KarmaType.NEGGYNEG,
    "poo": KarmaType.NEGGYNEG,
    "arrow_down": KarmaType.NEGGYNEG,
    "raised_hands": KarmaType.POZZYPOZ,
    "party_parrot": KarmaType.POZZYPOZ,
    "poop": KarmaType.NEGGYNEG,
    "shit": KarmaType.NEGGYNEG,
    "hankey": KarmaType.NEGGYNEG,
    "heavy_plus_sign": KarmaType.POZZYPOZ,
    "heavy_tick": KarmaType.POZZYPOZ,
    "heavy_minus_sign": KarmaType.NEGGYNEG
}

class AddKarmaReactionHandler(AbstractReactionHandler):

    @property
    def command(self) -> AddKarmaCommand:
        return AddKarmaCommand()
    
    def can_handle(self, reaction_add_event) -> bool:
        reaction = self.parse_reaction(reaction_add_event["reaction"])
        return reaction in _karma_reactions
    
    def _invoke_handler_logic(self, reaction_add_event):
        try:
            reaction = self.parse_reaction(reaction_add_event["reaction"])
            self.command.execute(awarded_to=reaction_add_event["reactee"],
                                 awarded_by=reaction_add_event["reactor"],
                                 reason="Reacted with " + reaction,
                                 karma_type=_karma_reactions[reaction])
            self._send_reaction_response(reaction_add_event)
        except Exception as ex:
            logging.exception(ex)

    def parse_reaction(self, raw_reaction):
        return raw_reaction.split('::')[0]
