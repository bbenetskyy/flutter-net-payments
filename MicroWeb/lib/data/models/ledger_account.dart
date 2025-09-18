import 'package:json_annotation/json_annotation.dart';

enum LedgerAccount {
  @JsonValue(1)
  cash('1'),
  @JsonValue(2)
  clearing('2');

  const LedgerAccount(this.value);

  final String value;

  @override
  String toString() => value;
}
