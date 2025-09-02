//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

enum VerificationStatus {
  @JsonValue(0)
  Pending('0'),
  @JsonValue(1)
  Rejected('1'),
  @JsonValue(2)
  Completed('2');

  const VerificationStatus(this.value);

  final String value;

  @override
  String toString() => value;
}
