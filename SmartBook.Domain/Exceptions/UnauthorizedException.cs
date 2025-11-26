using SmartBook.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Exceptions;
public class UnauthorizedException(string mesage) : Exception(mesage)
{
}

