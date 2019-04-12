# coding=utf-8
import datetime
import logging
import requests
import threading
import time

from flask import Flask

from slack_channel.initial_memberlist_populator import InitialMemberlistPopulator
from slack_channel.slack_event_handler import SlackEventHandler

app = Flask(__name__)


@app.route("/", methods=["GET", "POST"])
def start_up():
    return "Hello, I am bottomly."


@app.before_first_request
def activate_job():
    logging.basicConfig(filename="bottomly_" + str(datetime.date.today()) + ".log",
                        level=logging.INFO,
                        format="[%(asctime)s] %(levelname)s:%(message)s",
                        datefmt='%m/%d/%Y %H:%M:%S')
    def spin_up_slack_socket():
        handler = SlackEventHandler(app.debug)
        handler.handle_slack_context()

    thread = threading.Thread(target=spin_up_slack_socket)
    thread.start()


@app.route("/init_members", methods=["GET"])
def init_members():
    return InitialMemberlistPopulator().populate()


def start_runner():
    def start_loop():
        not_started = True
        while not_started:
            logging.info('In start loop')
            try:
                r = requests.get("http://localhost:5000/")
                if r.status_code == 200:
                    logging.info('Server started, quiting start_loop')
                    not_started = False
                print(r.status_code)
            except:
                logging.info('Server not yet started')
            time.sleep(2)

    print('Started runner')
    thread = threading.Thread(target=start_loop)
    thread.start()


if __name__ == '__main__':
    # start_runner()
    # app.run(debug=True)
    InitialMemberlistPopulator().populate()
    handler = SlackEventHandler(True)
    handler.handle_slack_context()
