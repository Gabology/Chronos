module Chronos.Data
open Chronos.Core

let saveShift workshift (f : WorkShift -> unit) = f workshift
