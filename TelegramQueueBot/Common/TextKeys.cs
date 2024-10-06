namespace TelegramQueueBot.Common
{
    public static class TextKeys
    {
        public const string DefaultHelp = "default_help";
        public const string Features_Help = "features_help";
        public const string CallingModeHelp = "calling_mode_help";

        public const string Start = "start";
        public const string SetSize = "set_size";
        public const string WrongSize = "wrong_size";
        public const string SizeIsSame = "size_is_same";
        public const string CurrentQueue = "current_queue";
        public const string CreatedQueue = "created_queue";
        public const string NoCreatedQueue = "no_created_queue";

        public const string QueueIsCallingUsers = "queue_is_calling_users";
        public const string QueueEndedCallingUsers = "queue_ended_calling_users";
        public const string NeedToTurnOnCallingMode = "need_to_turn_on_calling_mode";
        public const string FirstUserInQueue = "first_user_in_queue";
        public const string NextUserInQueue = "next_user_in_queue";
        public const string UserRecievingNotifications = "user_recieving_notifications";
        public const string UserNotRecievingNotifications = "user_not_recieving_notifications";

        public const string QueueIsEmpty = "queue_is_empty";
        public const string FirstUserDequeued = "first_user_dequeued";

        public const string QueueSavedAs = "queue_saved_as";
        public const string ChangedSavedQueueName = "changed_saved_queue_name";
        public const string QueueIsAlreadySaved = "queue_is_already_saved";
        public const string SavedQueuesList = "saved_queues_list";
        public const string NoSavedQueues = "no_saved_queues";

        public const string QueueMenu = "queue_menu";
        public const string ConfirmDeletion = "confirm_deletion";

        public const string RemovedAllBlankSpaces = "removed_all_blank_spaces";
        public const string NoBlankSpacesToRemove = "no_blank_spaces_to_remove";

        // chat veiw

        public const string ChangedChatView = "changed_chat_view";
        public const string ChatViewAuto = "chat_view_auto";
        public const string ChatViewTable = "chat_view_table";
        public const string ChatViewColumn = "chat_view_column";

        // jobs

        public const string JobsList = "jobs_list";
        public const string JobsListIsEmpty = "jobs_list_is_empty";
        public const string JobCreatedQueue = "job_created_queue";
        public const string NeedToSpecifyJobName = "need_to_specify_job_name";

        public const string JobMenu = "job_menu";
        public const string JobNextTime = "job_next_time";
        public const string SetInterval = "set_interval";

        public const string ConfirmJobDeletion = "confirm_job_deletion";
        public const string SelectJobQueueMenu = "select_job_queue_menu";
        public const string ScheduledQueue = "scheduled_queue";
        public const string ScheduledQueueAppeared = "scheduled_queue_appeared";

        public const string EnableChatNotifications = "enable_chat_notifications";
        public const string DisableChatNotifications = "disable_chat_notifications";

        public const string LeaveActionOutdated = "leave_action_outdated";

        // swap

        public const string UserNotAuthorized = "user_not_authorized";
        public const string SwapRequestCaption = "swap_request_caption";
        public const string SwapRequestSendedFirstUser = "swap_request_sended_first_user";
        public const string SwapRequestSendedSecondUser = "swap_request_sended_second_user";
        public const string SwapRequestSuccess = "swap_request_success";
        public const string SwapRequestDenied = "swap_request_denied";
        public const string SwapRequestOutdated = "swap_request_outdated";

        // buttons

        public const string LoadQueueBtn = "load_queue_btn";
        public const string DeleteQueueBtn = "delete_queue_btn";
        public const string ConfirmDeletionBtn = "confirm_deletion_btn";
        public const string BackBtn = "back_btn";
        public const string EmptyJobQueueBtn = "emtpy_job_queue_btn";
        public const string LoadJobWithQueueBtn = "load_job_with_queue_btn";
        public const string SelectedBtn = "selected_btn";
        public const string DoneBtn = "done_btn";
        public const string DenyBtn = "deny_btn";
        public const string MoveLeftBtn = "move_left_btn";
        public const string MoveRightBtn = "move_right_btn";
        public const string LeaveBtn = "leave_btn";
        public const string DefaultHelpBtn = "default_help_btn";
        public const string CallingModeHelpBtn = "calling_mode_btn";
        public const string FeaturesHelpBtn = "features_help_btn";
        public const string NeedToBeAuthorizedForSwap = "need_to_be_authorized_for_swap";
    }
}
