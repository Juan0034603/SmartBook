namespace SmartBook.Domain.Exceptions;

public class BadRequestException(string mesage) : Exception(mesage)
{
}
