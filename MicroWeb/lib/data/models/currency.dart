//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

enum Currency {
  @JsonValue(0)
  EUR('0'),
  @JsonValue(1)
  USD('1'),
  @JsonValue(2)
  PLN('2'),
  @JsonValue(3)
  GBP('3');

  const Currency(this.value);

  final String value;

  @override
  String toString() => value;
}
