Mofichan
========

.. image:: https://ci.appveyor.com/api/projects/status/0lnl92u04uxwtpyp/branch/develop?svg=true
   :target: https://ci.appveyor.com/project/TAGC/mofichan/branch/develop
   :alt: AppVeyor Build Status


Mofichan is a chatbot under recent development.

Unlike other chatbots, the goal isn't to just provide her with utility functions but to give her a personality as well.

Mofichan is designed with a modular architecture. Her behaviour is formed by arranging independent "behaviour modules" into
a chain of responsibility at run-time, and she is abstracted from the backend communications management allowing her to potentially work on various different platforms.

Currently, Mofichan only works on Discord.
