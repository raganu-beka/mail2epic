﻿using Blue.Mail2Epic.Models.Dtos.Email;

 namespace Blue.Mail2Epic.Models.Events;

public class EmailFetchedEvent
{
    public required EmailDataDto Email { get; set; }
    public required List<int> RecipientUserAccountIds { get; set; }
}