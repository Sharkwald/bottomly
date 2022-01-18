import logging
from model.karma import Karma, KarmaType

from commands import AddKarmaCommand
from slack_channel.abstract_reaction_handler import AbstractReactionHandler

_karma_reactions = {
    "joy": KarmaType.POZZYPOZ,
    "+1": KarmaType.POZZYPOZ,
    "thumbsup": KarmaType.POZZYPOZ,
    "clap": KarmaType.POZZYPOZ,
    "arrow_up": KarmaType.POZZYPOZ,
    "heart": KarmaType.POZZYPOZ,
    "smile": KarmaType.POZZYPOZ,
    "-1": KarmaType.NEGGYNEG,
    "thumbsdown": KarmaType.NEGGYNEG,
    "poo": KarmaType.NEGGYNEG,
    "arrow_down": KarmaType.NEGGYNEG,
    "raised_hands": KarmaType.POZZYPOZ
}

class AddKarmaReactionHandler(AbstractReactionHandler):

    @property
    def command(self) -> AddKarmaCommand:
        return AddKarmaCommand()
    
    def can_handle(self, reaction_add_event) -> bool:
        return reaction_add_event["reaction"] in _karma_reactions
    
    def _invoke_handler_logic(self, reaction_add_event):
        try:
            self.command.execute(awarded_to=reaction_add_event["reactee"],
                                 awarded_by=reaction_add_event["reactor"],
                                 reason="Reacted with " + reaction_add_event["reaction"],
                                 karma_type=_karma_reactions[reaction_add_event["reaction"]])
            self._send_reaction_response(reaction_add_event)
        except Exception as ex:
            logging.exception(ex)
