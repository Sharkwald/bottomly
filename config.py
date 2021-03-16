import os
from enum import Enum
from pymodm import connect


class ConfigKeys(Enum):
    google_api_key = "bottomly_google_api_key"
    google_cse_id = "bottomly_google_cse_id"
    mongo_conn_st = "bottomly_mongo_conn_str"
    slack_bot_token = "bottomly_slack_bot_token"
    prefix = "bottomly_prefix"
    giphy_api_key = "bottomly_giphy_api_key"
    env_key = "bottomly_env"


class Config(object):

    # Constants
    Connection = "bottomly"
    _key_err_messages = {
        ConfigKeys.google_api_key: 'Google API key is not configured',
        ConfigKeys.google_cse_id: 'Google custom search engine ID is not configured',
        ConfigKeys.mongo_conn_st: 'MongoDB connection string is not configured',
        ConfigKeys.slack_bot_token: 'Slack bot auth token is not configured',
        ConfigKeys.prefix: "Standard command prefix is not configured",
        ConfigKeys.giphy_api_key: "Giphy API key is not configured",
        ConfigKeys.env_key: "Environment mode is not configured"
    }

    # Functions
    @staticmethod
    def get_config_value(key):
        if key.value in os.environ:
            return os.environ.get(key.value)
        else:
            raise EnvironmentError(Config._key_err_messages[key])

    def connect_to_db(self):
        conn_str = self.get_config_value(ConfigKeys.mongo_conn_st)
        connect(conn_str, alias=Config.Connection)

    @staticmethod
    def get_prefix():
        key = ConfigKeys.prefix
        if key.value in os.environ:
            return os.environ.get(key.value)
        else:
            raise EnvironmentError(Config._key_err_messages[key])

    def __init__(self):
        super(Config, self).__init__()
