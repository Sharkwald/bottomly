import logging
from model.karma import Karma, KarmaType

from commands import AddKarmaCommand
from slack_channel.abstract_reaction_handler import AbstractReactionHandler

# TODO: Dictionary of reactions to karma types

class AddKarmaReactionHandler(AbstractReactionHandler):

    @property
    def command(self) -> AddKarmaCommand:
        return AddKarmaCommand()
    
    def can_handle(self, reaction_add_event) -> bool:
        return reaction_add_event["reaction"] == "joy"
    
    def _lookup_karma_type(self, reaction: str) -> KarmaType:
        if reaction == "joy":
            return KarmaType.POZZYPOZ

    def _invoke_handler_logic(self, reaction_add_event):
        try:
            self.command.execute(awarded_to=reaction_add_event["reactee"],
                                 awarded_by=reaction_add_event["reactor"],
                                 reason="Reacted with " + reaction_add_event["reaction"],
                                 karma_type=self._lookup_karma_type(reaction_add_event["reaction"]))
            self._send_reaction_response(reaction_add_event)
        except Exception as ex:
            logging.exception(ex)