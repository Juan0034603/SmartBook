using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Domain.Exceptions;
public class NotFoundException(string mesage) : Exception(mesage)
{
}

