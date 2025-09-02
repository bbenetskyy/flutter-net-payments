//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

enum VerificationAction {
  @JsonValue(0)
  NewUserCreated('0'),
  @JsonValue(1)
  UserAssignedToCard('1'),
  @JsonValue(2)
  CardPrinting('2'),
  @JsonValue(3)
  CardTermination('3'),
  @JsonValue(4)
  PaymentCreated('4'),
  @JsonValue(5)
  PaymentReverted('5');

  const VerificationAction(this.value);

  final String value;

  @override
  String toString() => value;
}
