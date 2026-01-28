using ZurichAPI.Models.DTOs;

namespace ZurichAPI.Models.Response;

public class ReplyResponse : BaseResponse
{
    public ReplyDTO? Result { get; set; }
}
