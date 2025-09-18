import 'package:json_annotation/json_annotation.dart';

enum LedgerType {
  @JsonValue(1)
  credit('1'),
  @JsonValue(2)
  debit('2');

  const LedgerType(this.value);

  final String value;

  @override
  String toString() => value;
}
