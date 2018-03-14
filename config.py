import os
from enum import Enum
from pymodm import connect


class ConfigKeys(Enum):
    google_api_key = "bottomly_google_api_key"
    google_cse_id = "bottomly_google_cse_id"
    mongo_conn_st = "bottomly_mongo_conn_str"


class Config(object):

    # Constants
    Connection = "bottomly"
    _key_err_messages = {
        ConfigKeys.google_api_key: 'Google API key is not configured',
        ConfigKeys.google_cse_id: 'Google custom search engine ID is not configured',
        ConfigKeys.mongo_conn_st: 'MongoDB connection string is not configured'
    }

    # Functions
    def get_config_value(self, key):
        if key.value in os.environ:
            return os.environ.get(key.value)
        else:
            raise EnvironmentError(Config._key_err_messages[key])

    def connect_to_db(self):
        conn_str = self.get_config_value(ConfigKeys.mongo_conn_st)
        connect(conn_str, alias=Config.Connection)


    def __init__(self):
        super(Config, self).__init__()
