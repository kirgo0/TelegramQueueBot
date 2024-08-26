﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramQueueBot.Common
{
    public static class TextKeys
    {
        public const string Start = "start";
        public const string SetSize = "set_size";
        public const string WrongSize = "wrong_size";
        public const string SizeIsSame = "size_is_same";
        public const string NewQueue = "new_queue";
        public const string CurrentQueue = "current_queue";
        public const string CreatedQueue = "created_queue";
        public const string NoCreatedQueue = "no_created_queue";

        public const string QueueIsCallingUsers = "queue_is_calling_users";
        public const string QueueEndedCallingUsers = "queue_ended_calling_users";
        public const string NeedToTurnOnCallingMode = "need_to_turn_on_calling_mode";

        public const string QueueIsEmpty = "queue_is_empty";
        public const string FirstUserDequeued = "first_user_dequeued";

        public const string QueueSavedAs = "queue_saved_as";
        public const string ChangedSavedQueueName = "changed_saved_queue_name";
        public const string QueueIsAlreadySaved = "queue_is_already_saved";
        public const string SavedQueuesList = "saved_queues_list";
        public const string NoSavedQueues = "no_saved_queues";

        public const string QueueMenu = "queue_menu";
        public const string LoadQueueBtn = "load_queue_btn";
        public const string DeleteQueueBtn = "delete_queue_btn";
        public const string ConfirmDeletion = "confirm_deletion";
        public const string ConfirmDeletionBtn = "confirm_deletion_btn";
        public const string BackBtn = "back_btn";

        public const string RemovedAllBlankSpaces = "removed_all_blank_spaces";
        public const string NoBlankSpacesToRemove = "no_blank_spaces_to_remove";

        public const string ChangedChatView = "changed_chat_view";
        public const string ChatViewAuto = "chat_view_auto";
        public const string ChatViewTable = "chat_view_table";
        public const string ChatViewColumn = "chat_view_column";


    }
}