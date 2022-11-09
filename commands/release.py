import logging
from github import Github

from config import ConfigKeys, Config
from commands.abstract_command import AbstractCommand


class ReleaseCommand(AbstractCommand):
    def get_purpose(self):
        return 'Describes the latest release of bottomly'

    def execute(self):
        try:
            g = Github(self.token)
            repo = g.get_user().get_repo('bottomly')
            release = repo.get_latest_release()
            release_desc = f'Latest Release: *{release.title}* v{release.tag_name}'
            release_desc += f'\n_Published at {release.published_at}_'
            if len(release.body) > 0:
                release += f'\n{release.body}'
            return release_desc
        except Exception as ex:
            logging.exception('Error retrieving release info from Github: ' + str(ex))
            return None

    def __init__(self):
        super(ReleaseCommand, self)
        config = Config()
        self.token = config.get_config_value(ConfigKeys.github_token)
